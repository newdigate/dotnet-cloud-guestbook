# .net guestbook 
```c#```, ```.net6.0```, ```aspnet-core```, ```kubernetes```, ```skaffold```, ```mongodb```, ```prometheus```, ```grafana```, ```loki```, ```serilog```

Using google [cloud-code](https://marketplace.visualstudio.com/items?itemName=GoogleCloudTools.cloudcode) sample [dotnet-guestbook](https://github.com/GoogleCloudPlatform/cloud-code-samples/tree/master/dotnet/dotnet-guestbook) code as a starting point, I've added some features:
  * a persistent database
  * prometheus instance for monitoring
  * grafana for visualization

*work in progress*

## requirements
* If running locally, you'll need a **container** or **virtual machine manager**
  * [minikube](https://minikube.sigs.k8s.io/docs/start/)
    * <a href="https://minikube.sigs.k8s.io/docs/drivers/docker/">Docker</a>
    | <a href="https://minikube.sigs.k8s.io/docs/drivers/hyperkit/">Hyperkit</a>
    | <a href="https://minikube.sigs.k8s.io/docs/drivers/hyperv/">Hyper-V</a>
    | <a href="https://minikube.sigs.k8s.io/docs/drivers/kvm2/">KVM</a>
    | <a href="https://minikube.sigs.k8s.io/docs/drivers/parallels/">Parallels</a>
    | <a href="https://minikube.sigs.k8s.io/docs/drivers/podman/">Podman</a>
    | <a href="https://minikube.sigs.k8s.io/docs/drivers/virtualbox/">VirtualBox</a>
    | <a href="https://minikube.sigs.k8s.io/docs/drivers/vmware/">VMware Fusion/Workstation</a>
  * [kubernetes in docker](https://docs.docker.com/desktop/kubernetes/)
  * [kind](https://kind.sigs.k8s.io/docs/user/quick-start/)
* For Kubernetes you'll need:
  * access to a running kubernetes cluster (local, or remote) via [kubectl](https://kubernetes.io/docs/tasks/tools/#kubectl) client
* [vscode](https://code.visualstudio.com/)
* vscode extension: [cloud-code](https://marketplace.visualstudio.com/items?itemName=GoogleCloudTools.cloudcode)
* 7 Oct 2023: using netcore debug helper image 
  * by default dotnet v7 is built using alpine base images
  * google cloud code was installing the debug adapters for netcore for the wrong architecture, alpine requires `linux-musc-amd64`, not `linux-amd64`. 
  * to use debug adapters for alpine based images, run: 
  ``` sh
  $ skaffold config set --global debug-helpers-registry registry.hub.docker.com/nicnewdigate
  ```
    * the source of the above image is here [newdigate/container-debug-support](https://github.com/newdigate/container-debug-support)
## getting started
### local cluster development
* open in visual studio code
* edit ```kubernetes/local/guestbook-persistent-volume.yaml```
  * change the `hostPath` to point to ``` persistence``` folder in the root of this repository
* from a command terminal, apply all kubernetes manifest yaml files in ```kubernetes/local```
``` sh
> cd persistence
> kubectl apply -f .
```
* from vscode cloud code panel, select ```run app``` or ```debug app```

## command line
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
> docker run --name mongodb --rm -v /Users/nicholasnewdigate/Development/guestbook-dotnet6/persistence/mongodb:/data/db -p 27017:27017 mongo:4
>  kubectl run dnsutils --image tutum/dnsutils -ti -- bash
```

* configure grafana
  * login to [grafana](http://127.0.0.1:3000) admin, password: bitnami
  * import prometheus datasource http://dotnet-guestbook-prometheus:9090
  * import grafana dashboards
    * [aspnet core exporter](https://grafana.com/grafana/dashboards/10915)
    * [node exporter full](https://grafana.com/grafana/dashboards/1860)

* opentelemetry
  * [getting-started](https://opentelemetry.io/docs/instrumentation/net/getting-started/)

## notes
* cloned from https://github.com/GoogleCloudPlatform/cloud-code-vscode
* FIX: OCI runtime exec failed: exec failed: container_linux.go:380: starting container process caused: no such file or directory: unknown
  * [container-debug-support](https://github.com/GoogleContainerTools/container-debug-support/issues/103#issuecomment-1000968907)

