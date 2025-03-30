output "vpc_id" {
  description = "The ID of the VPC"
  value       = aws_vpc.main.id
}

output "public_subnet_id_a" {
  description = "The ID of the public subnet in AZ a"
  value       = aws_subnet.public_a.id
}

output "public_subnet_id_b" {
  description = "The ID of the public subnet in AZ b"
  value       = aws_subnet.public_b.id
}

output "public_subnet_ids" {
  description = "List of all public subnet IDs"
  value       = [aws_subnet.public_a.id, aws_subnet.public_b.id]
}

output "private_subnet_id_a" {
  description = "The ID of the private subnet in AZ a"
  value       = aws_subnet.private_a.id
}

output "private_subnet_id_b" {
  description = "The ID of the private subnet in AZ b"
  value       = aws_subnet.private_b.id
}

output "private_subnet_ids" {
  description = "List of all private subnet IDs"
  value       = [aws_subnet.private_a.id, aws_subnet.private_b.id]
}

output "internet_gateway_id" {
  description = "The ID of the Internet Gateway"
  value       = aws_internet_gateway.igw.id
}

output "nat_gateway_id" {
  description = "The ID of the NAT Gateway"
  value       = aws_nat_gateway.nat.id
}

output "public_route_table_id" {
  description = "The ID of the public route table"
  value       = aws_route_table.public.id
}

output "private_route_table_id" {
  description = "The ID of the private route table"
  value       = aws_route_table.private.id
}