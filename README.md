# .net guestbook 
```c#```, ```.net6.0```, ```aspnet-core```, ```minikube```, ```skaffold```, ```mongodb```, ```prometheus```, ```grafana```

Using google [cloud-code](https://marketplace.visualstudio.com/items?itemName=GoogleCloudTools.cloudcode) sample [dotnet-guestbook](https://github.com/GoogleCloudPlatform/cloud-code-samples/tree/master/dotnet/dotnet-guestbook) code as a starting point, I've added some features:
  * a persistent database
  * prometheus instance for monitoring
  * grafana for visualization

*work in progress*

## requirements
* If running locally, you'll need a **container** or **virtual machine manager**
  * <a href="https://minikube.sigs.k8s.io/docs/drivers/docker/">Docker</a>
  | <a href="https://minikube.sigs.k8s.io/docs/drivers/hyperkit/">Hyperkit</a>
  | <a href="https://minikube.sigs.k8s.io/docs/drivers/hyperv/">Hyper-V</a>
  | <a href="https://minikube.sigs.k8s.io/docs/drivers/kvm2/">KVM</a>
  | <a href="https://minikube.sigs.k8s.io/docs/drivers/parallels/">Parallels</a>
  | <a href="https://minikube.sigs.k8s.io/docs/drivers/podman/">Podman</a>
  | <a href="https://minikube.sigs.k8s.io/docs/drivers/virtualbox/">VirtualBox</a>
  | <a href="https://minikube.sigs.k8s.io/docs/drivers/vmware/">VMware Fusion/Workstation</a>
* For Kubernetes you'll need:
  * access to a running kubernetes cluster (local, or remote) via [kubectl](https://kubernetes.io/docs/tasks/tools/#kubectl) client
  * for local access:
    * [minikube](https://minikube.sigs.k8s.io/docs/start/)
    * [kubernetes in docker](https://docs.docker.com/desktop/kubernetes/)
* [vscode](https://code.visualstudio.com/)
* vscode extension: [cloud-code](https://marketplace.visualstudio.com/items?itemName=GoogleCloudTools.cloudcode)

## getting started
* open in visual studio code
* edit ```kubernetes/monogdb-persistent-volume.yaml``` - change the hostPath to point to a directory which will store your mongodb database files
* from cloud code panel, select ```run app``` or ```debug app```

## debug and watch changes
``` sh
> cd dotnet-cloud-guestbook
> skaffold debug -v info --port-forward --auto-build --auto-deploy --auto-sync --rpc-http-port 57994 --filename skaffold.yaml --wait-for-deletions-max 2m0s --wait-for-connection
```

## some helpfull commands, urls
```sh
> minikube start
> minikube dashboard &
> cd src/backend/prometheus-config && minikube mount $(PWD):/opt/bitnami/prometheus/conf/ &
> minikube ssh -- docker system prune
> kubectl exec --stdin --tty container-name -- /bin/bash
> docker run --name proxy --rm -v /Users/nicholasnewdigate/Development/docker/proxy-cache:/cachedir -p 8000:8000 pmoust/squid-deb-proxy
```

* configure grafana
  * login to [grafana](http://127.0.0.1:3000) admin, password: bitnami
  * import prometheus datasource http://dotnet-guestbook-prometheus:9090
  * import grafana dashboard - https://grafana.com/grafana/dashboards/10915

* opentelemetry
  * [getting-started](https://opentelemetry.io/docs/instrumentation/net/getting-started/)

## notes
* cloned from https://github.com/GoogleCloudPlatform/cloud-code-vscode
* FIX: OCI runtime exec failed: exec failed: container_linux.go:380: starting container process caused: no such file or directory: unknown
  * [container-debug-support](https://github.com/GoogleContainerTools/container-debug-support/issues/103#issuecomment-1000968907)

