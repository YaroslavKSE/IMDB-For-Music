resource "aws_ssm_parameter" "parameter" {
  for_each = var.parameters

  name        = "/${var.environment}/${each.key}"
  description = lookup(each.value, "description", "Parameter for ${each.key}")
  type        = lookup(each.value, "type", "SecureString")
  value       = var.parameter_values[each.key]
  tier        = lookup(each.value, "tier", "Standard")

  tags = merge(
    var.common_tags,
    {
      Name = "/${var.environment}/${each.key}"
    }
  )
}