$key = cat $PSScriptRoot\..\private\NugetApiKey.txt

Publish-Module -Path $PSScriptRoot\..\module\import-csv2 -NugetApiKey $key -Verbose
