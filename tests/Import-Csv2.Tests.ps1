
BeforeAll {
    function New-DataFile {
        param(
            [string]$Data
        )

        $file = [IO.Path]::GetTempFileName()
        $data | Set-Content $file

        $file
    }

    class Rec {
        [int]$Int
        [string]$String
    }
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
        } | Should -Throw "*not enough*"
    }

    It "can map records to object instance by specifying a record type" {
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

    It "can accept the column name mapping" {
        $file = New-DataFile @"
a,b,c
10,xxx,1.0
20,yyy,2.0
"@
        $d = Import-Csv2 $file -AsDictionary -ColumnNameMap @{
            a = "XXX"
            c = "ZZZ"
        }

        $d.Count | Should -Be 3
        $d.Keys | Should -Be "XXX", "b", "ZZZ"

        $d["XXX"][0] | Should -Be 10
        $d["b"][1] | Should -Be "yyy"
    }

    It "can accept the integer value index as key of column name mapping" {
        $file = New-DataFile @"
a,b,c
10,xxx,1.0
20,yyy,2.0
"@
        $d = Import-Csv2 $file -AsDictionary -ColumnNameMap @{
            0 = "XXX"
            2 = "ZZZ"
        }

        $d.Count | Should -Be 3
        $d.Keys | Should -Be "XXX", "b", "ZZZ"

        $d["XXX"][0] | Should -Be 10
        $d["b"][1] | Should -Be "yyy"
    }

    It "can accept the type name mapping in combination with -RecordType" {
        $file = New-DataFile @"
a,b,c
10,xxx,1.0
20,yyy,2.0
"@
        $d = Import-Csv2 $file -RecordType Rec -ColumnNameMap @{
            "a" = "Int"
            "b" = "String"
        }

        $d.Length | Should -Be 2
        $d[0] | Should -BeOfType ([Rec])
        $d[0].Int | Should -Be 10
        $d[1].String | Should -Be "yyy"
    }

    It "can convert field values to a specified type" {
        $file = New-DataFile @"
Int,String,Double
10,xxx,1.0
20,yyy,2.0
"@
        $d = Import-Csv2 $file -AsDictionary -ColumnTypes @{ Int = [int]; Double = [double] }

        $d["Int"][0] | Should -BeOfType ([int])
        $d["Int"] | Should -Be 10, 20

        $d["String"][0] | Should -BeOfType ([string])
        $d["String"] | Should -Be "xxx", "yyy"

        $d["Double"][0] | Should -BeOfType ([double])
        $d["Double"] | Should -Be 1.0, 2.0
    }

    It "can accept the pipeline input" {
        $file = New-DataFile @"
a,b,c
10,xxx,1.0
20,yyy,2.0
30,zzz,2.0
"@
        $d = Get-Content $file | Import-Csv2

        $d.Count | Should -Be 3
        $d[0].a | Should -Be 10
        $d[1].b | Should -Be "yyy"
        $d[2].c | Should -Be 2.0
    }

    It "returns nothing for empty imput" {
        $d = Import-Csv2

        $d | Should -Be $null
    }
}
