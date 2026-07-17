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


RUN dotnet publish src/Comnix/Comnix.csproj \
    -p:DebugType=None \
    -c Release \
    -r linux-musl-x64 \
    -o /app \
    --self-contained true

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine

COPY --from=build /app/ /

RUN addgroup -g 1000 -S comnix \
    && adduser -u 1000 -S -G comnix comnix \
    && mkdir -p /app/config \
    && chown -R comnix:comnix /app/config

USER comnix

EXPOSE 5000

ENV ASPNETCORE_HTTP_PORTS=5000
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

ENTRYPOINT ["/Comnix"]
