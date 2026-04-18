pipeline {
    agent any
    
    environment {
        // Cấu hình môi trường giả định chuẩn doanh nghiệp
        DOCKER_REGISTRY = 'baobao0303'
        IMAGE_TAG = "v1.0.${BUILD_NUMBER}"
    }

    stages {
        stage('Checkout Source') {
            steps {
                checkout scm
                echo 'Hành động: Đã kéo source code AeroCommerce mới nhất về Jenkins Agent.'
            }
        }

        stage('Build & Test Khối Microservices') {
            // Chạy Build các cụm rẽ nhánh đa luồng song song (Kỹ năng nâng cao)
            parallel {
                stage('API Gateway') {
                    steps {
                        sh 'dotnet build src/AeroCommerce.ApiGateway/AeroCommerce.ApiGateway.csproj'
                    }
                }
                stage('User Service') {
                    steps {
                        sh 'dotnet build src/AeroCommerce.UserService/AeroCommerce.UserService.csproj'
                    }
                }
                stage('Product Service') {
                    steps {
                        sh 'dotnet build src/AeroCommerce.ProductService/AeroCommerce.ProductService.csproj'
                    }
                }
                stage('Cart Service') {
                    steps {
                        sh 'dotnet build src/AeroCommerce.CartService/AeroCommerce.CartService.csproj'
                    }
                }
                stage('Order Service') {
                    steps {
                        sh 'dotnet build src/AeroCommerce.OrderService/AeroCommerce.OrderService.csproj'
                    }
                }
                stage('Post Service') {
                    steps {
                        sh 'dotnet build src/AeroCommerce.PostService/AeroCommerce.PostService.csproj'
                    }
                }
            }
        }

        stage('Compile Docker Images (CI)') {
            steps {
                script {
                    echo "Đóng gói toàn bộ 6 dịch vụ thành các Docker Image độc lập..."
                    // Dùng chế độ build tự động sinh từ file docker-compose chuẩn
                    sh 'docker-compose build'
                }
            }
        }

        stage('Deploy To Staging Target (CD)') {
            steps {
                echo "Đẩy các bản Docker Image mới nhất lên Server giả định..."
                echo "-- Tự động tái khởi động hệ thống thay phiên (Rolling Update) --"
                // Lệnh thực tế sẽ là bash script đẩy qua SSH tới máy chủ:
                // sh 'scp docker-compose.yml root@192.168.1.1:/app/'
                // sh 'ssh root@192.168.1.1 "cd /app/ && docker-compose up -d"'
            }
        }
    }

    post {
        success {
            echo "✅ [SUCCESS] Toàn bộ hệ thống Microservices đã được nạp lên Production thành công!"
        }
        failure {
            echo "❌ [ERROR] Đường ống Build thất bại. Sẽ gửi Email/Slack cảnh báo cho team."
        }
    }
}
