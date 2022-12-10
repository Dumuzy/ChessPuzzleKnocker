set vers=1.0.1
set temp0=C:\tmp\cpp\
set packname=CPPecker_%vers%
set tempdir=%temp0%%packname%



md %tempdir%
del /Q %tempdir%\*.*
del /Q %tempdir%\..\*.zip


xcopy preconf-pzls\*.* %tempdir%
copy part-gzs\lic_part_puzzle-20000.csv.gz %tempdir%

cd src\ChessUI\bin\Debug\netcoreapp3.1
xcopy *.exe %tempdir%
xcopy  *.dll %tempdir% 
xcopy *.json %tempdir%

cd %tempdir%\..

7za a -tzip cpp  %packname%
rename cpp.zip %packname%.zip

del /Q %tempdir%\*.*
rd %tempdir%

echo off
echo "%packname%.zip created in %temp0%"
