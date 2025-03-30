resource "aws_ecs_cluster" "main" {
  name = "${var.environment}-${var.name}-cluster"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.environment}-${var.name}-cluster"
    }
  )
}

resource "aws_cloudwatch_log_group" "ecs_logs" {
  for_each = toset(var.services)

  name              = "/ecs/${var.environment}/${each.value}"
  retention_in_days = var.log_retention_days

  tags = merge(
    var.common_tags,
    {
      Name        = "/ecs/${var.environment}/${each.value}"
      Environment = var.environment
      Service     = each.value
    }
  )
}

# Create security group for ECS tasks
resource "aws_security_group" "ecs_tasks_sg" {
  name        = "${var.environment}-ecs-tasks-sg"
  description = "Security group for ECS tasks"
  vpc_id      = var.vpc_id

  # Allow all outbound
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
    description = "Allow all outbound traffic"
  }

  # Allow inbound from ALB
  ingress {
    from_port       = 80
    to_port         = 80
    protocol        = "tcp"
    security_groups = [var.alb_security_group_id]
    description     = "Allow HTTP traffic from ALB"
  }

  # Optional: Allow traffic between services if needed
  ingress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    self        = true
    description = "Allow all traffic between ECS tasks"
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.environment}-ecs-tasks-sg"
    }
  )
}

# IAM Role for ECS Tasks
resource "aws_iam_role" "ecs_task_execution_role" {
  name = "${var.environment}-ecs-task-execution-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })

  tags = var.common_tags
}

# Attach the necessary policies to the ECS Task Execution Role
resource "aws_iam_role_policy_attachment" "ecs_task_execution_role_policy" {
  role       = aws_iam_role.ecs_task_execution_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

# Policy to allow reading from SSM Parameter Store
resource "aws_iam_policy" "ecs_task_ssm_policy" {
  name        = "${var.environment}-ecs-task-ssm-policy"
  description = "Policy to allow ECS tasks to read from SSM Parameter Store"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "ssm:GetParameters",
          "ssm:GetParameter"
        ]
        Resource = [
          "arn:aws:ssm:${var.region}:${data.aws_caller_identity.current.account_id}:parameter/${var.environment}/*"
        ]
      }
    ]
  })

  tags = var.common_tags
}

resource "aws_iam_role_policy_attachment" "ecs_task_ssm_policy_attachment" {
  role       = aws_iam_role.ecs_task_execution_role.name
  policy_arn = aws_iam_policy.ecs_task_ssm_policy.arn
}

# IAM Role for ECS Task
resource "aws_iam_role" "ecs_task_role" {
  name = "${var.environment}-ecs-task-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })

  tags = var.common_tags
}

# Policy for task role - minimal permissions for now
resource "aws_iam_policy" "ecs_task_role_policy" {
  name        = "${var.environment}-ecs-task-role-policy"
  description = "Policy for ECS Task Role"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "logs:CreateLogStream",
          "logs:PutLogEvents"
        ]
        Resource = [for log_group in aws_cloudwatch_log_group.ecs_logs : "${log_group.arn}:*"]
      }
    ]
  })

  tags = var.common_tags
}

resource "aws_iam_role_policy_attachment" "ecs_task_role_policy_attachment" {
  role       = aws_iam_role.ecs_task_role.name
  policy_arn = aws_iam_policy.ecs_task_role_policy.arn
}

# Import submodules with individual task definitions
module "user_service" {
  source = "./tasks/user_service"

  environment = var.environment
  region      = var.region
  common_tags = var.common_tags

  ecs_task_execution_role_arn = aws_iam_role.ecs_task_execution_role.arn
  ecs_task_role_arn           = aws_iam_role.ecs_task_role.arn

  cloudwatch_log_group_name = aws_cloudwatch_log_group.ecs_logs["user-service"].name

  service_config = var.user_service_config
  domain_name    = var.domain_name

  # Connection string parameter
  postgres_connection_string_parameter = var.postgres_connection_string_parameter

  # Pass Auth0 credentials directly
  auth0_domain                  = var.auth0_domain
  auth0_client_id               = var.auth0_client_id
  auth0_client_secret           = var.auth0_client_secret
  auth0_audience                = var.auth0_audience
  auth0_management_api_audience = var.auth0_management_api_audience
}

module "music_catalog_service" {
  source = "./tasks/music_catalog_service"

  environment = var.environment
  region      = var.region
  common_tags = var.common_tags

  ecs_task_execution_role_arn = aws_iam_role.ecs_task_execution_role.arn
  ecs_task_role_arn           = aws_iam_role.ecs_task_role.arn

  cloudwatch_log_group_name = aws_cloudwatch_log_group.ecs_logs["music-catalog-service"].name

  service_config = var.music_catalog_service_config
  domain_name    = var.domain_name

  # Connection string parameters
  mongodb_connection_string_parameter = var.mongodb_connection_string_parameter
  redis_connection_string_parameter   = var.redis_connection_string_parameter

  # Pass Spotify credentials directly
  spotify_client_id     = var.spotify_client_id
  spotify_client_secret = var.spotify_client_secret
}

module "music_interaction_service" {
  source = "./tasks/music_interaction_service"

  environment = var.environment
  region      = var.region
  common_tags = var.common_tags

  ecs_task_execution_role_arn = aws_iam_role.ecs_task_execution_role.arn
  ecs_task_role_arn           = aws_iam_role.ecs_task_role.arn

  cloudwatch_log_group_name = aws_cloudwatch_log_group.ecs_logs["music-interaction-service"].name

  service_config = var.music_interaction_service_config
  domain_name    = var.domain_name

  # Connection string parameters
  postgres_connection_string_parameter = var.postgres_connection_string_parameter
  mongodb_connection_string_parameter  = var.mongodb_connection_string_parameter
}

# ECS Services
resource "aws_ecs_service" "user_service" {
  name                               = "${var.environment}-user-service"
  cluster                            = aws_ecs_cluster.main.id
  task_definition                    = module.user_service.task_definition_arn
  desired_count                      = var.user_service_config.desired_count
  launch_type                        = "FARGATE"
  platform_version                   = "LATEST"
  health_check_grace_period_seconds  = 60
  deployment_minimum_healthy_percent = 100
  deployment_maximum_percent         = 200

  network_configuration {
    security_groups  = [aws_security_group.ecs_tasks_sg.id]
    subnets          = var.private_subnet_ids
    assign_public_ip = false
  }

  load_balancer {
    target_group_arn = var.user_service_target_group_arn
    container_name   = "user-service"
    container_port   = 80
  }

  lifecycle {
    ignore_changes = [desired_count]
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.environment}-user-service"
    }
  )
}

resource "aws_ecs_service" "music_catalog_service" {
  name                               = "${var.environment}-music-catalog-service"
  cluster                            = aws_ecs_cluster.main.id
  task_definition                    = module.music_catalog_service.task_definition_arn
  desired_count                      = var.music_catalog_service_config.desired_count
  launch_type                        = "FARGATE"
  platform_version                   = "LATEST"
  health_check_grace_period_seconds  = 60
  deployment_minimum_healthy_percent = 100
  deployment_maximum_percent         = 200

  network_configuration {
    security_groups  = [aws_security_group.ecs_tasks_sg.id]
    subnets          = var.private_subnet_ids
    assign_public_ip = false
  }

  load_balancer {
    target_group_arn = var.music_catalog_service_target_group_arn
    container_name   = "music-catalog-service"
    container_port   = 80
  }

  lifecycle {
    ignore_changes = [desired_count]
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.environment}-music-catalog-service"
    }
  )
}

resource "aws_ecs_service" "music_interaction_service" {
  name                               = "${var.environment}-music-interaction-service"
  cluster                            = aws_ecs_cluster.main.id
  task_definition                    = module.music_interaction_service.task_definition_arn
  desired_count                      = var.music_interaction_service_config.desired_count
  launch_type                        = "FARGATE"
  platform_version                   = "LATEST"
  health_check_grace_period_seconds  = 60
  deployment_minimum_healthy_percent = 100
  deployment_maximum_percent         = 200

  network_configuration {
    security_groups  = [aws_security_group.ecs_tasks_sg.id]
    subnets          = var.private_subnet_ids
    assign_public_ip = false
  }

  load_balancer {
    target_group_arn = var.music_interaction_service_target_group_arn
    container_name   = "music-interaction-service"
    container_port   = 80
  }

  lifecycle {
    ignore_changes = [desired_count]
  }

  tags = merge(
    var.common_tags,
    {
      Name = "${var.environment}-music-interaction-service"
    }
  )
}

# Auto Scaling Configuration
resource "aws_appautoscaling_target" "user_service" {
  max_capacity       = var.user_service_config.max_capacity
  min_capacity       = var.user_service_config.min_capacity
  resource_id        = "service/${aws_ecs_cluster.main.name}/${aws_ecs_service.user_service.name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

resource "aws_appautoscaling_policy" "user_service_cpu" {
  name               = "${var.environment}-user-service-cpu-autoscaling"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.user_service.resource_id
  scalable_dimension = aws_appautoscaling_target.user_service.scalable_dimension
  service_namespace  = aws_appautoscaling_target.user_service.service_namespace

  target_tracking_scaling_policy_configuration {
    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }
    target_value       = 70.0
    scale_in_cooldown  = 300
    scale_out_cooldown = 60
  }
}

resource "aws_appautoscaling_policy" "user_service_memory" {
  name               = "${var.environment}-user-service-memory-autoscaling"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.user_service.resource_id
  scalable_dimension = aws_appautoscaling_target.user_service.scalable_dimension
  service_namespace  = aws_appautoscaling_target.user_service.service_namespace

  target_tracking_scaling_policy_configuration {
    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageMemoryUtilization"
    }
    target_value       = 80.0
    scale_in_cooldown  = 300
    scale_out_cooldown = 60
  }
}

resource "aws_appautoscaling_target" "music_catalog_service" {
  max_capacity       = var.music_catalog_service_config.max_capacity
  min_capacity       = var.music_catalog_service_config.min_capacity
  resource_id        = "service/${aws_ecs_cluster.main.name}/${aws_ecs_service.music_catalog_service.name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

resource "aws_appautoscaling_policy" "music_catalog_service_cpu" {
  name               = "${var.environment}-music-catalog-service-cpu-autoscaling"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.music_catalog_service.resource_id
  scalable_dimension = aws_appautoscaling_target.music_catalog_service.scalable_dimension
  service_namespace  = aws_appautoscaling_target.music_catalog_service.service_namespace

  target_tracking_scaling_policy_configuration {
    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }
    target_value       = 70.0
    scale_in_cooldown  = 300
    scale_out_cooldown = 60
  }
}

resource "aws_appautoscaling_policy" "music_catalog_service_memory" {
  name               = "${var.environment}-music-catalog-service-memory-autoscaling"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.music_catalog_service.resource_id
  scalable_dimension = aws_appautoscaling_target.music_catalog_service.scalable_dimension
  service_namespace  = aws_appautoscaling_target.music_catalog_service.service_namespace

  target_tracking_scaling_policy_configuration {
    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageMemoryUtilization"
    }
    target_value       = 80.0
    scale_in_cooldown  = 300
    scale_out_cooldown = 60
  }
}

resource "aws_appautoscaling_target" "music_interaction_service" {
  max_capacity       = var.music_interaction_service_config.max_capacity
  min_capacity       = var.music_interaction_service_config.min_capacity
  resource_id        = "service/${aws_ecs_cluster.main.name}/${aws_ecs_service.music_interaction_service.name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

resource "aws_appautoscaling_policy" "music_interaction_service_cpu" {
  name               = "${var.environment}-music-interaction-service-cpu-autoscaling"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.music_interaction_service.resource_id
  scalable_dimension = aws_appautoscaling_target.music_interaction_service.scalable_dimension
  service_namespace  = aws_appautoscaling_target.music_interaction_service.service_namespace

  target_tracking_scaling_policy_configuration {
    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }
    target_value       = 70.0
    scale_in_cooldown  = 300
    scale_out_cooldown = 60
  }
}

resource "aws_appautoscaling_policy" "music_interaction_service_memory" {
  name               = "${var.environment}-music-interaction-service-memory-autoscaling"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.music_interaction_service.resource_id
  scalable_dimension = aws_appautoscaling_target.music_interaction_service.scalable_dimension
  service_namespace  = aws_appautoscaling_target.music_interaction_service.service_namespace

  target_tracking_scaling_policy_configuration {
    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageMemoryUtilization"
    }
    target_value       = 80.0
    scale_in_cooldown  = 300
    scale_out_cooldown = 60
  }
}

# Get current AWS account ID
data "aws_caller_identity" "current" {}