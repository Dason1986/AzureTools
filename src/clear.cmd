@echo off
setlocal enabledelayedexpansion

set "binCount=0"
set "objCount=0"
set "coverageReportCount=0"
set "coverageFileCount=0"

echo "delete all bin folders"
for /d /r . %%d in (bin) do (
    if exist "%%d" (
        rd /s /q "%%d"
        set /a binCount+=1
    )
)

echo "delete all obj folders"
for /d /r . %%d in (obj) do (
    if exist "%%d" (
        rd /s /q "%%d"
        set /a objCount+=1
    )
)

echo "delete all CoverageReport folders"
for /d /r . %%d in (CoverageReport) do (
    if exist "%%d" (
        rd /s /q "%%d"
        set /a coverageReportCount+=1
    )
)

echo "delete all  coverage.opencover.xml files..."
for /r . %%f in (coverage.opencover.xml) do (
    if exist "%%f" (
        del /q "%%f"
        set /a coverageFileCount+=1
    )
)

echo.
echo "clear complete"
echo "delete statistics:"
echo "bin folders: !binCount! "
echo "obj folders: !objCount! "
echo "CoverageReport folders: !coverageReportCount! "
echo "coverage.opencover.xml files: !coverageFileCount! "

endlocal
