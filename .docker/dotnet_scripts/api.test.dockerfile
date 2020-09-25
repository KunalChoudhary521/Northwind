FROM mcr.microsoft.com/dotnet/core/sdk:3.1

WORKDIR /app
COPY ./Northwind.API/Northwind.API.csproj Northwind.API/
COPY ./Northwind.Data/Northwind.Data.csproj Northwind.Data/

RUN dotnet restore Northwind.API/Northwind.API.csproj

COPY . .

CMD ["/bin/bash", "-c", "dotnet run -p Northwind.API --no-launch-profile"]