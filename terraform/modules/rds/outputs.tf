output "rds_address" {
  description = "The hostname of the RDS instance"
  value       = aws_db_instance.main.address
}

output "rds_port" {
  description = "The port of the RDS instance"
  value       = aws_db_instance.main.port
}

output "rds_endpoint" {
  description = "The endpoint of the RDS instance"
  value       = aws_db_instance.main.endpoint
}

output "rds_security_group_id" {
  description = "The ID of the security group for the RDS instance"
  value       = aws_security_group.rds_sg.id
}

output "rds_db_name" {
  description = "The database name"
  value       = aws_db_instance.main.db_name
}

output "rds_username" {
  description = "The username for the database"
  value       = aws_db_instance.main.username
}

output "rds_password_parameter" {
  description = "The SSM Parameter Store key for the database password"
  value       = aws_ssm_parameter.db_password.name
}

output "rds_connection_string" {
  description = "The PostgreSQL connection string without password"
  value       = "postgresql://${aws_db_instance.main.username}:PASSWORD@${aws_db_instance.main.endpoint}/${aws_db_instance.main.db_name}"
  sensitive   = true
}