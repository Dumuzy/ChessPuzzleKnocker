
set temp0=C:\tmp\cpp\
set tempdir=%temp0%ChessPuzzlePecker


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

7za a -tzip ChessPuzzlePecker ChessPuzzlePecker

del /Q %tempdir%\*.*
rd %tempdir%

echo off
echo "ChessPuzzlePecker.zip created in %temp0%"
