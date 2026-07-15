FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

RUN apk add --no-cache \
    clang \
    build-base \
    zlib-dev

WORKDIR /src

COPY Directory.Packages.props .
COPY Directory.Build.props .
COPY src/Directory.Build.props src/
COPY src/Comnix/Comnix.csproj src/Comnix/

RUN dotnet restore src/Comnix/Comnix.csproj -r linux-musl-x64

COPY . .

RUN dotnet publish \
    src/Comnix/Comnix.csproj \
    -c Release \
    -r linux-musl-x64 \
    --no-restore \
    -o /app

FROM scratch

COPY --from=build /lib/ld-musl-x86_64.so.1 /lib/
COPY --from=build /app/ /

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["/Comnix"]