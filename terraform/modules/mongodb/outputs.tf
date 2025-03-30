output "project_id" {
  description = "MongoDB Atlas project ID"
  value       = mongodbatlas_project.main.id
}

output "cluster_id" {
  description = "MongoDB Atlas cluster ID"
  value       = mongodbatlas_advanced_cluster.main.cluster_id
}

output "cluster_name" {
  description = "MongoDB Atlas cluster name"
  value       = mongodbatlas_advanced_cluster.main.name
}

output "mongodb_connection_strings" {
  description = "MongoDB Atlas connection strings"
  value       = mongodbatlas_advanced_cluster.main.connection_strings
  sensitive   = true
}

output "mongodb_standard_connection_string" {
  description = "MongoDB Atlas standard connection string"
  value       = try(mongodbatlas_advanced_cluster.main.connection_strings[0].standard, "")
  sensitive   = true
}

output "mongodb_private_connection_string" {
  description = "MongoDB Atlas private connection string"
  value       = try(mongodbatlas_advanced_cluster.main.connection_strings[0].private_endpoint[0].srv_connection_string, "")
  sensitive   = true
}

output "db_username" {
  description = "MongoDB Atlas database username"
  value       = mongodbatlas_database_user.main.username
}

output "db_password_parameter" {
  description = "SSM Parameter Store key for MongoDB Atlas password"
  value       = aws_ssm_parameter.db_password.name
}

output "mongodb_connection_string_parameter" {
  description = "SSM Parameter Store key for MongoDB Atlas connection string"
  value       = aws_ssm_parameter.connection_string.name
}

output "privatelink_endpoint_id" {
  description = "MongoDB Atlas PrivateLink Endpoint ID"
  value       = mongodbatlas_privatelink_endpoint.main.private_link_id
}

output "privatelink_service_name" {
  description = "MongoDB Atlas PrivateLink Service Name"
  value       = mongodbatlas_privatelink_endpoint.main.endpoint_service_name
}

output "vpc_endpoint_id" {
  description = "AWS VPC Endpoint ID for MongoDB Atlas"
  value       = aws_vpc_endpoint.mongodb.id
}

output "security_group_id" {
  description = "Security Group ID for MongoDB Atlas VPC Endpoint"
  value       = aws_security_group.mongodb_endpoint.id
}

output "atlas_service_name" {
  description = "MongoDB Atlas PrivateLink Service Name for VPC Endpoint"
  value       = mongodbatlas_privatelink_endpoint.main.endpoint_service_name
}