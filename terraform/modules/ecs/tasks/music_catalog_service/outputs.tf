output "task_definition_arn" {
  description = "ARN of the Music Catalog Service task definition"
  value       = aws_ecs_task_definition.music_catalog_service.arn
}

output "task_definition_family" {
  description = "Family of the Music Catalog Service task definition"
  value       = aws_ecs_task_definition.music_catalog_service.family
}

output "container_name" {
  description = "The name of the container in the Music Catalog Service task definition"
  value       = "music-catalog-service"
}