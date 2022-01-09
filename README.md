# .net guestbook with Cloud Code, Prometheus, Grafana
*  *work in progress*

* `minikube start`
* `minikube mount src/backend/prometheus-config:/opt/bitnami/prometheus/conf &`
* `minikube dashboard &`

* configure grafana
  * login to [grafana](http://127.0.0.1:3000) admin, password: bitnami
  * import prometheus datasource http://dotnet-guestbook-prometheus:9090
  * import grafana dashboard - https://grafana.com/grafana/dashboards/10915

* cloned from https://github.com/GoogleCloudPlatform/cloud-code-vscode

