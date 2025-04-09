#!/bin/bash
cd ${DEPLOY_DIR}
echo "Stopping service: ${SERVICE}"
docker compose down ${SERVICE}
echo "Pulling latest image for: ${SERVICE}"
docker compose pull ${SERVICE}
echo "Starting service: ${SERVICE}"
docker compose up -d ${SERVICE}
echo "Deployment completed for: ${SERVICE}"
