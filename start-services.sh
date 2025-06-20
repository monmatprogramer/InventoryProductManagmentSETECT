#!/bin/bash
# Bash script to start all InventoryPro services
# This script starts all required services for the InventoryPro application

echo -e "\033[32mStarting InventoryPro Services...\033[0m"

# Service definitions
declare -A services=(
    ["AuthService"]="src/Services/InventoryPro.AuthService:5141"
    ["ProductService"]="src/Services/InventoryPro.ProductService:5089" 
    ["SalesService"]="src/Services/InventoryPro.SalesService:5282"
    ["ReportService"]="src/Services/InventoryPro.ReportService:5179"
    ["Gateway"]="src/Gateway/InventoryPro.Gateway:5000"
)

# Function to check if port is in use
check_port() {
    local port=$1
    if netstat -tuln 2>/dev/null | grep -q ":$port "; then
        return 0  # Port is in use
    else
        return 1  # Port is free
    fi
}

# Array to store PIDs
declare -a pids=()

# Function to cleanup on exit
cleanup() {
    echo -e "\n\033[31mStopping all services...\033[0m"
    for pid in "${pids[@]}"; do
        if kill -0 "$pid" 2>/dev/null; then
            kill "$pid" 2>/dev/null
            echo "Stopped service with PID $pid"
        fi
    done
    echo -e "\033[31mAll services stopped.\033[0m"
    exit 0
}

# Set trap to cleanup on exit
trap cleanup SIGINT SIGTERM EXIT

# Start each service
for service_name in "${!services[@]}"; do
    IFS=':' read -r service_path service_port <<< "${services[$service_name]}"
    
    echo -e "\033[33mStarting $service_name on port $service_port...\033[0m"
    
    # Check if port is already in use
    if check_port "$service_port"; then
        echo -e "\033[36m$service_name appears to already be running on port $service_port\033[0m"
        continue
    fi
    
    # Start the service in background
    cd "$service_path" || {
        echo -e "\033[31mError: Could not change to directory $service_path\033[0m"
        continue
    }
    
    dotnet run --no-launch-profile > "../../../logs/${service_name,,}.log" 2>&1 &
    local pid=$!
    pids+=("$pid")
    
    echo -e "\033[32m$service_name started (PID: $pid)\033[0m"
    
    # Return to root directory
    cd - > /dev/null
    
    # Wait a bit between starting services
    sleep 2
done

echo -e "\n\033[32mAll services started!\033[0m"
echo -e "\033[36mServices running:\033[0m"

for service_name in "${!services[@]}"; do
    IFS=':' read -r service_path service_port <<< "${services[$service_name]}"
    echo -e "  - $service_name: http://localhost:$service_port"
done

echo -e "\n\033[33mPress Ctrl+C to stop all services\033[0m"

# Wait for services to finish or user interruption
wait