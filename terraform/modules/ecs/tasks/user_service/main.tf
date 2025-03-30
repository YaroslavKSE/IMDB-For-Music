resource "aws_ecs_task_definition" "user_service" {
  family                   = "${var.environment}-user-service"
  execution_role_arn       = var.ecs_task_execution_role_arn
  task_role_arn            = var.ecs_task_role_arn
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = var.service_config.cpu
  memory                   = var.service_config.memory

  container_definitions = jsonencode([
    {
      name      = "user-service"
      image     = "${var.service_config.ecr_repository_url}:${var.service_config.image_tag}"
      essential = true

      portMappings = [
        {
          containerPort = 80
          hostPort      = 80
          protocol      = "tcp"
        }
      ]

      # Standard environment variables
      environment = [
        { name = "ASPNETCORE_ENVIRONMENT", value = var.environment == "prod" ? "Production" : "Development" },
        { name = "FRONTEND_BASE_URL", value = var.environment == "prod" ? "https://${var.domain_name}" : "https://dev.${var.domain_name}" },

        # Auth0 configuration (all directly in environment variables)
        { name = "AUTH0_DOMAIN", value = var.auth0_domain },
        { name = "AUTH0_CLIENT_ID", value = var.auth0_client_id },
        { name = "AUTH0_CLIENT_SECRET", value = var.auth0_client_secret },
        { name = "AUTH0_AUDIENCE", value = var.auth0_audience },
        { name = "AUTH0_MANAGEMENT_API_AUDIENCE", value = var.auth0_management_api_audience }
      ]

      # Access connection strings from parameter store
      secrets = [
        # Full connection string for PostgreSQL
        { name = "ConnectionStrings__DefaultConnection", valueFrom = var.postgres_connection_string_parameter }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = var.cloudwatch_log_group_name
          "awslogs-region"        = var.region
          "awslogs-stream-prefix" = "user-service"
        }
      }

      healthCheck = {
        command     = ["CMD-SHELL", "wget -q -O - http://localhost/health || exit 1"]
        interval    = 30
        timeout     = 5
        retries     = 3
        startPeriod = 60
      }
    }
  ])

  tags = merge(
    var.common_tags,
    {
      Name = "${var.environment}-user-service"
    }
  )
}