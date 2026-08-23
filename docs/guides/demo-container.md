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

## Caddy and Blazor boot assets

The demo host publishes the framework boot script through .NET static web
assets. A normal Caddy reverse proxy needs no special route or rewrite:

```caddyfile
fancy.example.com {
    reverse_proxy fancyblazor-demo:8080
}
```

If the browser reports a `404` for `/_framework/blazor.web.js`, first verify
the application directly rather than changing Caddy:

```sh
curl -I http://127.0.0.1:8080/_framework/blazor.web.js
```

The response must be `200 OK` with a JavaScript content type. The demo project
sets `RequiresAspNetWebAssets` and its Docker build asserts that the published
file exists. Rebuild and redeploy the image if the direct request returns 404;
the reverse proxy can only relay the application's response.

## Crawler discovery

The demo maps its permissive `/robots.txt` and generated `/sitemap.xml` through
`SyntaxCircus.AspNetCore.Common`, covering the homepage and every demo route.
Preserve both endpoints through the reverse proxy; they make the full catalog
discoverable to search engines and compliant AI crawlers. They do not guarantee
that a particular AI product will fetch or index the site immediately.
