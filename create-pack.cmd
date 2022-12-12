rem
rem     This script packs together a ChessKnocker_...zip package.
rem     It takes the version number from ChessKnocker.csproj. 
rem     To work, this script needs   tclsh.exe and 7za.exe in PATH. 

@echo off
set projfile=src\ChessUI\ChessKnocker.csproj
rem The following seems to be the way to go to put the result of a command into 
rem a variable in cmd-scripts. It is truly astonishing. 
for /f usebackq %%i in (`tclsh extract-vers.tcl %projfile%`) do (
  set vers=%%i
)

set vers=1.0.1
set temp0=C:\tmp\ckno\
set packname=ChessKnocker_%vers%
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
