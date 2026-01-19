# Trivy Operator Dashboard Tips & Tricks

This section provides practical guidance on how to get more out of the Trivy Operator Dashboard, especially for workflows not yet supported directly in the UI.

## Scan on Demand

Until on‑demand scanning is available in the application, you can trigger a scan using a simple Kubernetes Deployment. The idea is to create:
- an init container that runs indefinitely, and
- a main container that references the image you want scanned.

Because the init container never completes, the main container never starts. This allows Trivy to scan the image safely without ever executing it.

Example:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: simple-deployment
spec:
  replicas: 1
  selector:
    matchLabels:
      app: simple-app
  template:
    metadata:
      labels:
        app: simple-app
    spec:
      initContainers:
      - name: wait-forever
        image: alpine:latest
        command: ["sh", "-c", "tail -f /dev/null"]
        terminationGracePeriodSeconds: 1
      containers:
      - name: to-be-scanned-image
        image: my-image:my-tag
```

### Why This Approach Is the Safest - and Also the Simplest

This pattern delivers the strongest security posture while staying extremely simple to use. The init container never completes, which guarantees that the main container (the untrusted image) **never executes**, fully isolating it from the runtime environment. At the same time, the setup is just a tiny Deployment with no extra configuration, making it **the most straightforward way to trigger a scan**. Trivy still detects the image reference and generates all relevant reports, and those reports remain available for up to 24 hours (the default TTL) after the Deployment is scaled down.

## Using Path in Ingress

The application fully supports being served under an ingress path, but there is one important caveat: **a URL rewrite must be performed**. Unfortunately, the exact rewrite configuration differs between ingress controllers.

Below is an example for the **NGINX Ingress Controller**, showing only the relevant parts:
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  annotations:
    [...]
    nginx.ingress.kubernetes.io/rewrite-target: /$2
    nginx.ingress.kubernetes.io/use-regex: "true"
  [...]
spec:
  ingressClassName: nginx
  rules:
  - host: my-host
    http:
      paths:
      - backend:
          service:
            name: trivy-operator-dashboard
            port:
              number: 8900
        path: /my-path(/|$)(.*)
        pathType: ImplementationSpecific
  [...]
```

The important ones are
- the two annotations
  - `nginx.ingress.kubernetes.io/rewrite-target: /$2`
  - `nginx.ingress.kubernetes.io/use-regex: "true"`
- the path must use a regex pattern such as `/my-path(/|$)(.*)` (replace `/my-path` with whatever path you want)
- the `pathType` must be set to `ImplementationSpecific` so the regex is interpreted correctly.

Once configured, the application will be accessible at (important: note the trailing `/`):
```
https://my-host/my-path/
```

### Additional note

Other ingress controllers also support URL rewrite functionality. Traefik, HAProxy, Istio, and several others provide mechanisms for path rewriting, though each uses its own syntax and configuration model. Because implementations differ, checking the documentation for your specific ingress controller is the most reliable approach.