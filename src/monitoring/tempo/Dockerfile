FROM bitnami/grafana-tempo:latest
COPY tempo.yaml /etc/tempo.yaml
COPY overrides.yaml /etc/overrides.yaml
CMD [ "-config.file=/etc/tempo.yaml", "-distributor.log-received-spans.enabled"]