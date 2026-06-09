pipeline {


agent any

environment {
    IMAGE = "product-api:${BUILD_NUMBER}"
    NETWORK = "app-net"
    MYSQL_CONT = "app-mysql"
    API_CONT = "product-api"
    MYSQL_PWD = "root"
    MYSQL_DB = "internshipportal"
}

stages {

    stage('Checkout') {
        steps {
            checkout scm
        }
    }

    stage('Build Docker Image') {
        steps {
            bat 'docker build -t %IMAGE% .'
        }
    }

    stage('Create Network') {
        steps {
            bat '''
            docker network create %NETWORK% >nul 2>&1 || exit /b 0
            '''
        }
    }

    stage('Start MySQL') {
        steps {
            bat '''
            docker rm -f %MYSQL_CONT% >nul 2>&1

            docker run -d --name %MYSQL_CONT% --network %NETWORK% ^
            -e MYSQL_ROOT_PASSWORD=%MYSQL_PWD% ^
            -e MYSQL_DATABASE=%MYSQL_DB% ^
            mysql:8.0
            '''
        }
    }

    stage('Wait For MySQL') {
        steps {
            powershell 'Start-Sleep -Seconds 30'
        }
    }

    stage('Run API') {
        steps {
            bat '''
            docker rm -f %API_CONT% >nul 2>&1

            docker run -d --name %API_CONT% --network %NETWORK% ^
            -e ConnectionStrings__DefaultConnection="server=%MYSQL_CONT%;port=3306;database=%MYSQL_DB%;user=root;password=%MYSQL_PWD%" ^
            -p 5000:8080 ^
            %IMAGE%
            '''
        }
    }
}


}
