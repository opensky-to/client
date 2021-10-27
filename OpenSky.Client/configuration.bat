@echo Copying %2 application configuration pre build: "%1\app.config"
@copy /Y "%1\app.config.%2" "%1\app.config"