    @echo on
    setlocal enableextensions disabledelayedexpansion

    ::Arguments of either PreBuild or PostBuild
    set buildType=%1
    ::License key replacement file
    set sourceFile=%2
    
    ::Replacement string
    set DummyKey=##SyncfusionLicense##
    set LicenseKey=%SyncfusionLicenseKey%
    
    ::Replacement statement
    if NOT "%LicenseKey%" == "" (
        if "%buildType%" == "PostBuild" (
             echo Removing Syncfusion license key post build
             powershell -Command "(gc %sourceFile%) -Replace '%LicenseKey%','%DummyKey%'|SC %sourceFile%"
        )
        if "%buildType%" == "PreBuild" (
             echo Inserting Syncfusion license key pre build
             powershell -Command "(gc %sourceFile%) -Replace '%DummyKey%','%LicenseKey%'|SC %sourceFile%"
        )
    )