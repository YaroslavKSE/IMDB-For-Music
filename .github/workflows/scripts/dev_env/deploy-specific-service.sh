#!/bin/bash
cd ${DEPLOY_DIR}
echo "Stopping service: ${ACTUAL_SERVICE}"
docker compose down ${ACTUAL_SERVICE}
echo "Pulling latest image for: ${ACTUAL_SERVICE}"
docker compose pull ${ACTUAL_SERVICE}
echo "Starting service: ${ACTUAL_SERVICE}"
docker compose up -d ${ACTUAL_SERVICE}
echo "Deployment completed for: ${ACTUAL_SERVICE}"
