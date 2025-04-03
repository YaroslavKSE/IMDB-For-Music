output "cloudfront_domain_name" {
  description = "The domain name of the CloudFront distribution"
  value       = aws_cloudfront_distribution.frontend.domain_name
}

output "cloudfront_id" {
  description = "The ID of the CloudFront distribution"
  value       = aws_cloudfront_distribution.frontend.id
}

output "cloudfront_arn" {
  description = "The ARN of the CloudFront distribution"
  value       = aws_cloudfront_distribution.frontend.arn
}

output "cloudfront_hosted_zone_id" {
  description = "The CloudFront distribution hosted zone ID"
  value       = aws_cloudfront_distribution.frontend.hosted_zone_id
}

output "cloudfront_distribution" {
  description = "The CloudFront distribution object"
  value       = aws_cloudfront_distribution.frontend
}