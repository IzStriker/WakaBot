# Run the following query on the database and export the results to a CSV file
# SELECT DISTINCT DiscordId, WakaName FROM Users;

# Import the CSV file
$users = Import-Csv -Path "Users.csv" -Header "DiscordId", "WakaName"

# remove the header row from the CSV file
$users = $users[1..$users.Count]
$results = @()

foreach($user in $users) {
    $url = "https://wakatime.com/api/v1/users/$($user.WakaName)/stats/"

    $response = Invoke-WebRequest -Uri $url
    $json = ConvertFrom-Json $response.Content
    $userId = $json.data.user_id

    $results += [PSCustomObject]@{
        UserId = $userId
        DiscordId = $user.DiscordId
        WakaName = $user.WakaName
    }
}

$sql = "INSERT INTO WakaUsers (Id,Username,usingOAuth) VALUES `n"
foreach($result in $results) {
    $sql += "('$($result.UserId)', '$($result.WakaName)', 0),`n"
}
$sql = $sql.TrimEnd(",", "`n") + ";"
Write-Output $sql

Write-Output ""

# write sql to Update the DiscordUsers table where Id = DiscordId to set the WakaUserId to UserId
foreach($result in $results) {
    $sql = "UPDATE DiscordUsers SET WakaUserId = '$($result.UserId)' WHERE Id = '$($result.DiscordId)';"
    Write-Output $sql
}