var builder = DistributedApplication.CreateBuilder(args);

var sshServer = builder.AddContainer("ssh-server", "lscr.io/linuxserver/openssh-server", "latest")
    .WithEndpoint(name: "ssh", port: 2222, targetPort: 2222)
    .WithEnvironment("PUID", "1000")
    .WithEnvironment("PGID", "1000")
    .WithEnvironment("USER_NAME", "test")
    .WithEnvironment("USER_PASSWORD", "pass123")
    .WithEnvironment("PASSWORD_ACCESS", "true");

builder.AddDockerfile(name: "comnix", contextPath: "../../", dockerfilePath: "src/Comnix/Dockerfile")
    .WithHttpEndpoint(port: 5000, targetPort: 5000)
    .WithBindMount("volumes/comnix/config", "/app/config")
    .WithImage("comnix:latest")
    .WaitFor(sshServer);

await builder.Build().RunAsync();
