# Demo container deployment

The interactive FancyBlazor demo is published from the `main` branch as a
public GitHub Container Registry image:

```bash
docker pull ghcr.io/syntax-circus/fancyblazor-demo:latest
docker run --detach --name fancyblazor-demo --publish 8080:8080 ghcr.io/syntax-circus/fancyblazor-demo:latest
```

The application listens on port `8080` inside the container. Put it behind
your own reverse proxy for HTTPS, a public hostname, and any access policy.

`latest` follows the newest successful `main` build. For a repeatable
deployment or rollback, use the immutable commit tag shown by the workflow:

```bash
docker pull ghcr.io/syntax-circus/fancyblazor-demo:sha-<commit-sha>
docker run --detach --name fancyblazor-demo --publish 8080:8080 ghcr.io/syntax-circus/fancyblazor-demo:sha-<commit-sha>
```

The image runs as the .NET runtime image's non-root application user. It does
not terminate TLS or include a reverse proxy.
