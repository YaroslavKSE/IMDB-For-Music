#!/bin/bash
cd ${DEPLOY_DIR}
echo "Stopping all services"
docker compose down
echo "Pulling latest images"
docker compose pull
echo "Starting all services"
docker compose up -d
echo "Deployment completed for all services"
