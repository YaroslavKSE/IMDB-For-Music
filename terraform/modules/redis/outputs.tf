output "redis_endpoint" {
  description = "The address of the Redis cluster endpoint"
  value       = aws_elasticache_cluster.redis.cache_nodes[0].address
}

output "redis_port" {
  description = "The port number of the Redis cluster endpoint"
  value       = aws_elasticache_cluster.redis.cache_nodes[0].port
}

output "redis_security_group_id" {
  description = "The ID of the security group for the Redis cluster"
  value       = aws_security_group.redis_sg.id
}

output "redis_parameter_group_id" {
  description = "The ID of the parameter group for the Redis cluster"
  value       = aws_elasticache_parameter_group.redis.id
}

output "redis_connection_string" {
  description = "The Redis connection string"
  value       = "redis://${aws_elasticache_cluster.redis.cache_nodes[0].address}:${aws_elasticache_cluster.redis.cache_nodes[0].port}"
}

output "redis_connection_string_parameter" {
  description = "The SSM Parameter Store key for the Redis connection string"
  value       = aws_ssm_parameter.redis_connection_string.name
}

output "redis_endpoint_parameter" {
  description = "The SSM Parameter Store key for the Redis endpoint"
  value       = aws_ssm_parameter.redis_endpoint.name
}