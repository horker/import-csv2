
function New-DataFile {
    param(
        [string]$Data
    )

    $file = [IO.Path]::GetTempFileName()
    $data | Set-Content $file

    $file
}

Describe "Test Import-Csv2" {

    It "can import a CSV file" {
        $file = New-DataFile @"
aa,bb,cc
10,20,30
11,21,31
"@
        $i = 0
        Import-Csv2 $file | foreach {
            if ($i -eq 0) {
                $_.aa | Should -Be "10"
                $_.bb | Should -Be "20"
                $_.cc | Should -Be "30"
            }
            else {
                $_.aa | Should -Be "11"
                $_.bb | Should -Be "21"
                $_.cc | Should -Be "31"
            }
            ++$i
        }
    }

    It "can load a white space separated file" {
        $file = New-DataFile @"
xxx yyy zzz
10  20  30
11  21  31
"@
        $d = Import-Csv2 $file -Delimiter ' ' -AsDictionary
        $d["xxx"] | Should -Be "10", "11"
    }

    It "can skip comment lines with the -AllowComments paramter" {
        $file = New-DataFile @"
xxx,yyy,zzz
10,20,30
# comment
11,21,31
#comment2
"@
        $d = Import-Csv2 $file -AllowComments -AsDictionary
        $d["xxx"] | Should -Be "10", "11"
    }

    It "can read a file without a header record" {
        $file = New-DataFile @"
10,20,30
11,21,31
"@
        $d = Import-Csv2 $file -NoHeader -AsDictionary
        $d["Column1"] | Should -Be "10", "11"
    }

    It "can read more columns than the header indicates" {
        $file = New-DataFile @"
xxx,yyy
10,20,30
11,21,31
"@
        $d = Import-Csv2 $file -AsDictionary
        $d["Column3"] | Should -Be "30", "31"
    }

    It "fills insufficient columns with emtpy strings" {
        $file = New-DataFile @"
xxx,yyy
10,20
11
"@
        $d = Import-Csv2 $file -AsDictionary
        $d["yyy"] | Should -Be "20", ""
    }

    It "raise an error when -Strict is active and the number of columns are insufficient" {
        $file = New-DataFile @"
xxx,yyy
10,20
11
"@
        {
            $ErrorActionPreference = "Stop"
            Import-Csv2 $file -Strict
        } | Should -Throw "not enough"
    }

    class Rec {
        [int]$Int
        [string]$String

    }

    It "can map records to object instance by specifying a type" {
        $file = New-DataFile @"
Int,String,Double
10,xxx,1.0
20,yyy,2.0
"@
        $d = Import-Csv2 $file -RecordType ([Rec])

        $d.Length | Should -Be 2
        $d[0] | Should -BeOfType ([Rec])
        $d[0].Int | Should -Be 10
        $d[1].String | Should -Be "yyy"
    }
}
