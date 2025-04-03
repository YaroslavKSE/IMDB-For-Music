data "aws_route53_zone" "main" {
  name         = var.domain_name
  private_zone = false
}

# A Record - Root domain to CloudFront
resource "aws_route53_record" "root_a" {
  zone_id = data.aws_route53_zone.main.zone_id
  name    = var.domain_name
  type    = "A"

  alias {
    name                   = var.cloudfront_domain_name
    zone_id                = var.cloudfront_hosted_zone_id
    evaluate_target_health = false
  }
}

# AAAA Record - Root domain to CloudFront (for IPv6)
resource "aws_route53_record" "root_aaaa" {
  count = var.enable_ipv6 ? 1 : 0

  zone_id = data.aws_route53_zone.main.zone_id
  name    = var.domain_name
  type    = "AAAA"

  alias {
    name                   = var.cloudfront_domain_name
    zone_id                = var.cloudfront_hosted_zone_id
    evaluate_target_health = false
  }
}

# A Record - www subdomain to CloudFront (if enabled)
resource "aws_route53_record" "www_a" {
  count = var.create_www_subdomain ? 1 : 0

  zone_id = data.aws_route53_zone.main.zone_id
  name    = "www.${var.domain_name}"
  type    = "A"

  alias {
    name                   = var.cloudfront_domain_name
    zone_id                = var.cloudfront_hosted_zone_id
    evaluate_target_health = false
  }
}

# AAAA Record - www subdomain to CloudFront (if enabled and IPv6 is enabled)
resource "aws_route53_record" "www_aaaa" {
  count = var.create_www_subdomain && var.enable_ipv6 ? 1 : 0

  zone_id = data.aws_route53_zone.main.zone_id
  name    = "www.${var.domain_name}"
  type    = "AAAA"

  alias {
    name                   = var.cloudfront_domain_name
    zone_id                = var.cloudfront_hosted_zone_id
    evaluate_target_health = false
  }
}

# A Record - API subdomain to ALB (if enabled)
resource "aws_route53_record" "api_a" {
  count = var.create_api_records ? 1 : 0

  zone_id = data.aws_route53_zone.main.zone_id
  name    = "${var.api_subdomain}.${var.domain_name}"
  type    = "A"

  alias {
    name                   = var.alb_domain_name
    zone_id                = var.alb_hosted_zone_id
    evaluate_target_health = true
  }
}

# AAAA Record - API subdomain to ALB (if enabled and IPv6 is enabled)
resource "aws_route53_record" "api_aaaa" {
  count = var.create_api_records && var.enable_ipv6 ? 1 : 0

  zone_id = data.aws_route53_zone.main.zone_id
  name    = "${var.api_subdomain}.${var.domain_name}"
  type    = "AAAA"

  alias {
    name                   = var.alb_domain_name
    zone_id                = var.alb_hosted_zone_id
    evaluate_target_health = true
  }
}