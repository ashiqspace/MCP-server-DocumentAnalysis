# Quick Deploy Script
# This script uses the detailed deployment script with your environment settings

# REQUIRED: Set your subscription ID here
$SUBSCRIPTION_ID = "911c9609-9522-467a-afe8-c58da5edd697"

# Run the deployment
.\infra\deploy.ps1 -SubscriptionId $SUBSCRIPTION_ID
