#!/bin/bash

# CategorySelectionTests.csの修正
sed -i '' '
# StudentDto -> ApiResponse<TestStudentDto> with .Data access
s/var student = await createResponse\.Content\.ReadFromJsonAsync<StudentDto>();/var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();\n        var student = studentResponse!.Data!;/g
s/var student = await createResponse\.Content\.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();/var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();\n        var student = studentResponse!.Data!;/g

# problem access with .Data
s/problem!\.CalculationTypeText/problem!.Data!.CalculationTypeText/g
s/problem\.Should\(\)\.NotBeNull\(\);$/problemResponse.Should().NotBeNull();\n        var problem = problemResponse!.Data;/g

# stats access with .Data
s/var stats = await .*\.ReadFromJsonAsync<ApiResponse<TestStatisticsDto>>();/var statsResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TestStatisticsDto>>();\n        var stats = statsResponse!.Data!;/g

# result access with .Data
s/var result = await .*\.ReadFromJsonAsync<ApiResponse<TestLearningRecordsResponseDto>>();/var resultResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TestLearningRecordsResponseDto>>();\n        var result = resultResponse!.Data!;/g
' CategorySelectionTests.cs

# CategoryPerformanceTests.csの修正
sed -i '' '
# StudentDto -> ApiResponse<TestStudentDto> with .Data access
s/var student = await createResponse\.Content\.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();/var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();\n        var student = studentResponse!.Data!;/g

# problem access with .Data
s/problem!\.CalculationTypeText/problem!.Data!.CalculationTypeText/g
s/var problem = await .*\.ReadFromJsonAsync<ApiResponse<TestProblemDto>>();/var problemResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TestProblemDto>>();\n        var problem = problemResponse!.Data!;/g

# stats access with .Data
s/var stats = await .*\.ReadFromJsonAsync<ApiResponse<TestStatisticsDto>>();/var statsResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TestStatisticsDto>>();\n        var stats = statsResponse!.Data!;/g
' CategoryPerformanceTests.cs

echo "Tests fixed successfully"
