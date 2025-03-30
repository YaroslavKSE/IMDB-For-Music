output "task_definition_arn" {
  description = "ARN of the User Service task definition"
  value       = aws_ecs_task_definition.user_service.arn
}

output "task_definition_family" {
  description = "Family of the User Service task definition"
  value       = aws_ecs_task_definition.user_service.family
}

output "container_name" {
  description = "The name of the container in the User Service task definition"
  value       = "user-service"
}