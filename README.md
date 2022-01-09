# .net guestbook 
c#, .net6.0, aspnet-core, minikube, skaffold, mongodb, prometheus, grafana

*work in progress*

## requirements
* Container or virtual machine manager -
  <a href="https://minikube.sigs.k8s.io/docs/drivers/docker/">Docker</a>
  <a href="https://minikube.sigs.k8s.io/docs/drivers/hyperkit/">Hyperkit</a>
  <a href="https://minikube.sigs.k8s.io/docs/drivers/hyperv/">Hyper-V</a>
  <a href="https://minikube.sigs.k8s.io/docs/drivers/kvm2/">KVM</a>
  <a href="https://minikube.sigs.k8s.io/docs/drivers/parallels/">Parallels</a>
  <a href="https://minikube.sigs.k8s.io/docs/drivers/podman/">Podman</a>
  <a href="https://minikube.sigs.k8s.io/docs/drivers/virtualbox/">VirtualBox</a>
  <a href="https://minikube.sigs.k8s.io/docs/drivers/vmware/">VMware Fusion/Workstation</a>
* [minikube](https://minikube.sigs.k8s.io/docs/start/)
* [vscode](https://code.visualstudio.com/)
* vscode extension [cloud-code](https://marketplace.visualstudio.com/items?itemName=GoogleCloudTools.cloudcode)

## some helpfull commands, urls
```sh
> minikube start
> minikube dashboard &
> cd src/backend/prometheus-config && minikube mount $(PWD):/opt/bitnami/prometheus/conf/ &
> minikube ssh -- docker system prune
> kubectl exec --stdin --tty container-name -- /bin/bash


```

* configure grafana
  * login to [grafana](http://127.0.0.1:3000) admin, password: bitnami
  * import prometheus datasource http://dotnet-guestbook-prometheus:9090
  * import grafana dashboard - https://grafana.com/grafana/dashboards/10915

## notes
* cloned from https://github.com/GoogleCloudPlatform/cloud-code-vscode
* FIX: OCI runtime exec failed: exec failed: container_linux.go:380: starting container process caused: no such file or directory: unknown
  * [container-debug-support](https://github.com/GoogleContainerTools/container-debug-support/issues/103#issuecomment-1000968907)
