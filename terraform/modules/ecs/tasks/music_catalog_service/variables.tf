variable "environment" {
  description = "Environment name (e.g., dev, prod)"
  type        = string
}

variable "region" {
  description = "AWS region"
  type        = string
}

variable "common_tags" {
  description = "Common tags to be applied to all resources"
  type        = map(string)
  default     = {}
}

variable "ecs_task_execution_role_arn" {
  description = "ARN of the ECS task execution role"
  type        = string
}

variable "ecs_task_role_arn" {
  description = "ARN of the ECS task role"
  type        = string
}

variable "cloudwatch_log_group_name" {
  description = "Name of the CloudWatch log group for the service"
  type        = string
}

variable "service_config" {
  description = "Configuration for the service"
  type = object({
    ecr_repository_url = string
    image_tag          = string
    cpu                = number
    memory             = number
  })
}

variable "domain_name" {
  description = "Domain name of the application"
  type        = string
}

variable "mongodb_connection_string_parameter" {
  description = "SSM Parameter path for MongoDB connection string"
  type        = string
}

variable "redis_connection_string_parameter" {
  description = "SSM Parameter path for Redis connection string"
  type        = string
}

# Spotify variables
variable "spotify_client_id" {
  description = "Spotify Client ID"
  type        = string
  sensitive   = true
}

variable "spotify_client_secret" {
  description = "Spotify Client Secret"
  type        = string
  sensitive   = true
}