$ErrorActionPreference = 'Stop'

$base = 'http://localhost:5000'

function Invoke-Json([string]$method, [string]$url, [string]$json) {
  return Invoke-WebRequest -UseBasicParsing -Method $method -Uri $url -ContentType 'application/json' -Body $json
}

function Get-ErrorStatus([object]$err) {
  if ($null -ne $err.Exception -and $null -ne $err.Exception.Response) {
    return [int]$err.Exception.Response.StatusCode
  }
  return $null
}

$invResp = Invoke-Json 'Post' ($base + '/api/therapist-invitations') '{}'
if ([int]$invResp.StatusCode -ne 201) { throw "CreateTherapistInvitation expected 201, got $($invResp.StatusCode)" }
$invObj = $invResp.Content | ConvertFrom-Json
if (-not $invObj.success) { throw 'CreateTherapistInvitation expected success=true' }
if ($null -eq $invObj.data -or [string]::IsNullOrWhiteSpace($invObj.data.code)) { throw 'CreateTherapistInvitation did not return data.code' }
$code = $invObj.data.code

$therEmail = 'ayse.yilmaz@example.com'
$therPassword = 'Test123!Pass'
$therJson = '{ "code": "' + $code.ToLower() + '", "email": "' + $therEmail + '", "password": "' + $therPassword + '", "firstName": "Ayse", "lastName": "Yilmaz", "graduationDate": "2015-06-15T00:00:00Z", "birthDate": "1990-02-10T00:00:00Z", "location": "Istanbul" }'
$therResp = Invoke-Json 'Post' ($base + '/api/therapists/register') $therJson
if ([int]$therResp.StatusCode -ne 201) { throw "RegisterTherapist expected 201, got $($therResp.StatusCode)" }
$therWrap = $therResp.Content | ConvertFrom-Json
if (-not $therWrap.success) { throw 'RegisterTherapist expected success=true' }
$therObj = $therWrap.data
if ($null -eq $therObj -or [string]::IsNullOrWhiteSpace($therObj.id)) { throw 'RegisterTherapist did not return data.id' }
$therId = $therObj.id

$codeResp = Invoke-Json 'Post' ($base + '/api/therapists/' + $therId + '/generate-code') '{ "aphasiaType": "Broca" }'
if ([int]$codeResp.StatusCode -ne 201) { throw "GeneratePatientCode expected 201, got $($codeResp.StatusCode)" }
$codeWrap = $codeResp.Content | ConvertFrom-Json
if (-not $codeWrap.success) { throw 'GeneratePatientCode expected success=true' }
$codeObj = $codeWrap.data
if ($null -eq $codeObj -or [string]::IsNullOrWhiteSpace($codeObj.code)) { throw 'GeneratePatientCode did not return data.code' }
$code = $codeObj.code
if ($codeObj.aphasiaType -ne 'Broca') { throw "Expected Broca aphasiaType, got $($codeObj.aphasiaType)" }

$patEmail = 'mehmet.demir@example.com'
$patPassword = 'Test123!Pass'
$patResp = Invoke-Json 'Post' ($base + '/api/patients/register-with-code') ('{ "email": "' + $patEmail + '", "password": "' + $patPassword + '", "firstName": "Mehmet", "lastName": "Demir", "birthDate": "1985-01-20T00:00:00Z", "location": "Ankara", "code": "' + $code + '" }')
if ([int]$patResp.StatusCode -ne 201) { throw "RegisterWithCode expected 201, got $($patResp.StatusCode)" }
$patWrap = $patResp.Content | ConvertFrom-Json
if (-not $patWrap.success) { throw 'RegisterWithCode expected success=true' }
$patObj = $patWrap.data
if ($patObj.therapistId -ne $therId) { throw "RegisterWithCode therapistId mismatch: expected $therId, got $($patObj.therapistId)" }
$patientId = $patObj.id
if ($patObj.aphasiaType -ne 'Broca') { throw "Expected Broca aphasiaType on patient, got $($patObj.aphasiaType)" }

# reuse should be 400 (hard delete)
$reuseStatus = $null
try {
  Invoke-Json 'Post' ($base + '/api/patients/register-with-code') ('{ "email": "veli.kaya@example.com", "password": "Test123!Pass", "firstName": "Veli", "lastName": "Kaya", "birthDate": "1992-11-03T00:00:00Z", "location": "Izmir", "code": "' + $code + '" }') | Out-Null
  $reuseStatus = 201
} catch {
  $reuseStatus = Get-ErrorStatus $_
}
if ($reuseStatus -ne 400) { throw "Reuse expected 400, got $reuseStatus" }

# therapist detail includes patient
$detailWrap = Invoke-RestMethod -Method Get -Uri ($base + '/api/therapists/' + $therId)
if (-not $detailWrap.success) { throw 'GetTherapist expected success=true' }
$detail = $detailWrap.data
$found = $false
foreach ($p in $detail.patients) {
  if ($p.id -eq $patientId) { $found = $true }
}
if (-not $found) { throw 'Patient not found in therapist detail' }

# login smoke
$loginWrap = Invoke-RestMethod -Method Post -Uri ($base + '/api/auth/login') -ContentType 'application/json' -Body ('{ "email": "' + $therEmail + '", "password": "' + $therPassword + '" }')
if (-not $loginWrap.success) { throw 'Login expected success=true' }
if ($null -eq $loginWrap.data -or [string]::IsNullOrWhiteSpace($loginWrap.data.token)) { throw 'Login did not return token' }

Write-Output 'SMOKE TEST PASSED'

