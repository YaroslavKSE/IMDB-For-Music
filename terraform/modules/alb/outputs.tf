output "alb_id" {
  description = "The ID of the Application Load Balancer"
  value       = aws_lb.api_alb.id
}

output "alb_arn" {
  description = "The ARN of the Application Load Balancer"
  value       = aws_lb.api_alb.arn
}

output "alb_dns_name" {
  description = "The DNS name of the Application Load Balancer"
  value       = aws_lb.api_alb.dns_name
}

output "alb_zone_id" {
  description = "The zone ID of the Application Load Balancer"
  value       = aws_lb.api_alb.zone_id
}

output "user_service_target_group_arn" {
  description = "The ARN of the user service target group"
  value       = aws_lb_target_group.user_service.arn
}

output "music_catalog_target_group_arn" {
  description = "The ARN of the music catalog service target group"
  value       = aws_lb_target_group.music_catalog_service.arn
}

output "rating_service_target_group_arn" {
  description = "The ARN of the rating service target group"
  value       = aws_lb_target_group.rating_service.arn
}

output "https_listener_arn" {
  description = "The ARN of the HTTPS listener"
  value       = aws_lb_listener.https.arn
}

output "security_group_id" {
  description = "The ID of the ALB security group"
  value       = aws_security_group.alb_sg.id
}