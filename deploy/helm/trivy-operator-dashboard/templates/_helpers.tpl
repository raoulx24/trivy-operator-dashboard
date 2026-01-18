{{/*
Expand the name of the chart.
*/}}
{{- define "trivy-operator-dashboard.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "trivy-operator-dashboard.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "trivy-operator-dashboard.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "trivy-operator-dashboard.labels" -}}
helm.sh/chart: {{ include "trivy-operator-dashboard.chart" . }}
{{ include "trivy-operator-dashboard.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "trivy-operator-dashboard.selectorLabels" -}}
app.kubernetes.io/name: {{ include "trivy-operator-dashboard.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "trivy-operator-dashboard.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "trivy-operator-dashboard.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}



{{- define "trivy-operator-dashboard.kubeConfigAllValuesSet" -}}
{{- and .Values.kubernetes.kubeConfigFileName .Values.kubernetes.kubeConfigPath .Values.kubernetes.kubeConfigSecretName -}}
{{- end -}}

{{- define "trivy-operator-dashboard.kubeConfigFullPath" -}}
{{- if include "trivy-operator-dashboard.kubeConfigAllValuesSet" . }}
{{- printf "%s/%s" .Values.kubernetes.kubeConfigPath .Values.kubernetes.kubeConfigFileName -}}
{{- end -}}
{{- end -}}


{{- define "trivy-operator-dashboard.fileRepositoryAllValuesSet" -}}
{{- and .Values.fileRepository.enabled .Values.fileRepository.pvcName .Values.fileRepository.basePath -}}
{{- end -}}
