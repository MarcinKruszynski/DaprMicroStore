# DaprMicroStore

My experiment with Dapr and Tye. Just 4 fun. 

Sample microservice app using:

- .NET Core 3.1
- ASP.NET Core 3.1
- Blazor
- Docker
- Kubernetes
- Dapr
- Tye
- Redis (pub-sub & state store)
- Kafka 


Tye Dapr Extension for Dapr 0.8.0 workaround for locally running services: copy components/*.yaml to %UserProfile%\.dapr\components 
https://github.com/dotnet/tye/issues/555
Run Kafka locally without Tye: docker-compose -f ./docker-compose-single-kafka.yml up -d
https://github.com/dotnet/tye/issues/464

To be continued
