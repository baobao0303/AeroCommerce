#!/bin/bash

# Màu sắc hiển thị terminal
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

echo -e "${GREEN}==============================================="
echo -e "🚀 BẮT ĐẦU CHẠY CÁC MICROSERVICES MODE WATCH (HOT-RELOAD)"
echo -e "===============================================${NC}\n"

# Hàm format log để dễ phân biệt
# Hàm này thêm Prefix [ServiceName] vào mỗi dòng xuất ra
run_service() {
    local prefix=$1
    local color=$2
    shift 2 # Bỏ 2 params đầu, giữ lại phần command
    "$@" 2>&1 | while IFS= read -r line; do
        echo -e "${color}[${prefix}]${NC} ${line}"
    done
}

# Khởi chạy CodePulse.ApiGateway (Cổng 5500 hoặc 5000)
run_service "ApiGateway" "$CYAN" dotnet watch --project src/CodePulse.ApiGateway/CodePulse.ApiGateway.csproj &
PID_GATEWAY=$!

# Khởi chạy CodePulse.UserService (Cổng 5001)
run_service "UserService" "$BLUE" dotnet watch --project src/CodePulse.UserService/CodePulse.UserService.csproj &
PID_USER=$!

# Khởi chạy CodePulse.PostService (Cổng 5002)
# Chờ 2s để cho ApiGateway/User lên trước
sleep 2 
run_service "PostService" "$YELLOW" dotnet watch --project src/CodePulse.PostService/CodePulse.PostService.csproj &
PID_POST=$!

# Khởi chạy CodePulse.ProductService (Cổng 5003)
run_service "ProductService" "$GREEN" dotnet watch --project src/CodePulse.ProductService/CodePulse.ProductService.csproj &
PID_PRODUCT=$!

# Khởi chạy CodePulse.CartService (Cổng 5004)
run_service "CartService" "$CYAN" dotnet watch --project src/CodePulse.CartService/CodePulse.CartService.csproj &
PID_CART=$!

# Khởi chạy CodePulse.OrderService (Cổng 5005)
run_service "OrderService" "$BLUE" dotnet watch --project src/CodePulse.OrderService/CodePulse.OrderService.csproj &
PID_ORDER=$!

# Dọn dẹp tiến trình rác khi ấn Ctrl+C
cleanup() {
    echo -e "\n${YELLOW}Đang tắt các Microservices...${NC}"
    kill $PID_GATEWAY $PID_USER $PID_POST $PID_PRODUCT $PID_CART $PID_ORDER 2>/dev/null
    pkill -f 'CodePulse' 2>/dev/null
    pkill -f 'dotnet' 2>/dev/null
    wait $PID_GATEWAY $PID_USER $PID_POST $PID_PRODUCT $PID_CART $PID_ORDER 2>/dev/null
    echo -e "${GREEN}Tất cả service đã được tắt an toàn!${NC}"
    exit 0
}

# Lắng nghe sự kiện Ctrl+C
trap cleanup SIGINT SIGTERM

echo -e "Các Services đang chạy nền... Bấm ${YELLOW}Ctrl+C${NC} để dừng chạy."
wait
