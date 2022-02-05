```
$ eval $(minikube docker-env)
$ docker build -t my-loki .
$ docker run -p 3100:3100 my-loki
```