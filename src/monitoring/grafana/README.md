```
$ eval $(minikube docker-env)
$ docker build -t my-grafana .
$ docker run -p 3000:3000 my-grafana
```