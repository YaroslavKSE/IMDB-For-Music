output "zone_id" {
  description = "The Route53 hosted zone ID"
  value       = data.aws_route53_zone.main.zone_id
}

output "domain_name" {
  description = "The domain name configured"
  value       = var.domain_name
}

output "domain_website_endpoint" {
  description = "The root domain endpoint"
  value       = "https://${var.domain_name}"
}

output "www_domain_website_endpoint" {
  description = "The www domain endpoint"
  value       = var.create_www_subdomain ? "https://www.${var.domain_name}" : null
}

output "root_a_record_name" {
  description = "The name of the root A record"
  value       = aws_route53_record.root_a.name
}

output "www_a_record_name" {
  description = "The name of the www A record"
  value       = var.create_www_subdomain ? aws_route53_record.www_a[0].name : null
}