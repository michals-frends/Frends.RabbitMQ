# Frends.RabbitMQ.Read
Frends RabbitMQ task to read message(s) from RabbitMQ queue.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT) 
[![Build](https://github.com/FrendsPlatform/Frends.RabbitMQ/actions/workflows/Read_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.RabbitMQ/actions)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.RabbitMQ.Read)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.RabbitMQ/Frends.RabbitMQ.Read|main)

# Installing

You can install the Task via frends UI Task View or you can find the NuGet package from the following NuGet feed https://www.myget.org/F/frends-tasks/api/v2.

## Building


Rebuild the project

`dotnet build`

Run tests

You will need access to RabbitMQ queue. Create command for docker: docker run -d --hostname my-rabbit -p 5672:5672 -p 8080:1567 -e RABBITMQ_DEFAULT_USER=agent -e RABBITMQ_DEFAULT_PASS=agent123  rabbitmq:3.7-management

`dotnet test`


Create a NuGet package

`dotnet pack --configuration Release`