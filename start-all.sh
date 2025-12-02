#!/bin/bash
# CodePulse Microservices - Start All Services

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "🚀 Starting CodePulse Microservices..."
echo ""

# Start UserService (port 5001)
echo "👤 Starting UserService on http://localhost:5001 ..."
dotnet run --project "$SCRIPT_DIR/src/CodePulse.UserService" &
USER_PID=$!

sleep 2

# Start PostService (port 5002)
echo "📝 Starting PostService on http://localhost:5002 ..."
dotnet run --project "$SCRIPT_DIR/src/CodePulse.PostService" &
POST_PID=$!

sleep 2

# Start ProductService (port 5003)
echo "🛍️ Starting ProductService on http://localhost:5003 ..."
dotnet run --project "$SCRIPT_DIR/src/CodePulse.ProductService" &
PRODUCT_PID=$!

sleep 2

# Start CartService (port 5004)
echo "🛒 Starting CartService on http://localhost:5004 ..."
dotnet run --project "$SCRIPT_DIR/src/CodePulse.CartService" &
CART_PID=$!

sleep 2

# Start OrderService (port 5005)
echo "📦 Starting OrderService on http://localhost:5005 ..."
dotnet run --project "$SCRIPT_DIR/src/CodePulse.OrderService" &
ORDER_PID=$!

sleep 2

# Start ApiGateway (port 5000)
echo "🚪 Starting ApiGateway on http://localhost:5000 ..."
dotnet run --project "$SCRIPT_DIR/src/CodePulse.ApiGateway" &
GW_PID=$!

echo ""
echo "✅ All services started!"
echo ""
echo "  API Gateway    → http://localhost:5000"
echo "  UserService    → http://localhost:5001/swagger"
echo "  PostService    → http://localhost:5002/swagger"
echo "  ProductService → http://localhost:5003/swagger"
echo "  CartService    → http://localhost:5004/swagger"
echo "  OrderService   → http://localhost:5005/swagger"
echo ""
echo "Press Ctrl+C to stop all services..."

# Wait and cleanup
trap "echo ''; echo 'Stopping all services...'; kill $USER_PID $POST_PID $PRODUCT_PID $CART_PID $ORDER_PID $GW_PID 2>/dev/null; pkill -f 'CodePulse' 2>/dev/null; pkill -f 'dotnet' 2>/dev/null; exit" INT TERM
wait
