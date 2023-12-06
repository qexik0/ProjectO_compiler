; ModuleID = 'ProjectO module'
source_filename = "ProjectO module"

define i32 @"Integer.Plus%Integer%"(ptr %0, i32 %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %sum = add i32 %thisInt, %1
  ret i32 %sum
}

define double @"Integer.Plus%Real%"(ptr %0, double %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %thisReal = sitofp i32 %thisInt to double
  %sum = fadd double %thisReal, %1
  ret double %sum
}

define i32 @"Integer.UnaryMinus%%"(ptr %0) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %thisNeg = sub i32 0, %thisInt
  ret i32 %thisNeg
}

define double @"Integer.ToReal%%"(ptr %0) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %thisReal = sitofp i32 %thisInt to double
  ret double %thisReal
}

define i1 @"Integer.ToBoolean%%"(ptr %0) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %isNonZero = icmp ne i32 %thisInt, 0
  ret i1 %isNonZero
}

define void @"Integer%Integer%"(ptr %0, i32 %1) {
entry:
  store i32 %1, ptr %0, align 4
  ret void
}

define void @"Integer%Real%"(ptr %0, double %1) {
entry:
  %res = fptosi double %1 to i32
  store i32 %res, ptr %0, align 4
  ret void
}

define i32 @"Integer.Minus%Integer%"(ptr %0, i32 %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %res = sub i32 %thisInt, %1
  ret i32 %res
}

define double @"Integer.Minus%Real%"(ptr %0, double %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %thisReal = sitofp i32 %thisInt to double
  %res = fsub double %thisReal, %1
  ret double %res
}

define i32 @"Integer.Mult%Integer%"(ptr %0, i32 %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %res = mul i32 %thisInt, %1
  ret i32 %res
}

define double @"Integer.Mult%Real%"(ptr %0, double %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %res = sitofp i32 %thisInt to double
  %res1 = fmul double %res, %1
  ret double %res1
}

define i32 @"Integer.Div%Integer%"(ptr %0, i32 %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %res = sdiv i32 %thisInt, %1
  ret i32 %res
}

define double @"Integer.Div%Real%"(ptr %0, double %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %thisReal = sitofp i32 %thisInt to double
  %res = fdiv double %thisReal, %1
  ret double %res
}

define i32 @"Integer.Rem%Integer%"(ptr %0, i32 %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %res = srem i32 %thisInt, %1
  ret i32 %res
}

define i1 @"Integer.Less%Integer%"(ptr %0, i32 %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %res = icmp slt i32 %thisInt, %1
  ret i1 %res
}

define i1 @"Integer.LessEqual%Integer%"(ptr %0, i32 %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %res = icmp sle i32 %thisInt, %1
  ret i1 %res
}

define i1 @"Integer.Greater%Integer%"(ptr %0, i32 %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %res = icmp sgt i32 %thisInt, %1
  ret i1 %res
}

define i1 @"Integer.GreaterEqual%Integer%"(ptr %0, i32 %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %res = icmp sge i32 %thisInt, %1
  ret i1 %res
}

define i1 @"Integer.Equal%Integer%"(ptr %0, i32 %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %res = icmp eq i32 %thisInt, %1
  ret i1 %res
}

define i1 @"Integer.Less%Real%"(ptr %0, double %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %thisReal = sitofp i32 %thisInt to double
  %res = fcmp olt double %thisReal, %1
  ret i1 %res
}

define i1 @"Integer.LessEqual%Real%"(ptr %0, double %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %thisReal = sitofp i32 %thisInt to double
  %res = fcmp ole double %thisReal, %1
  ret i1 %res
}

define i1 @"Integer.Greater%Real%"(ptr %0, double %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %thisReal = sitofp i32 %thisInt to double
  %res = fcmp ogt double %thisReal, %1
  ret i1 %res
}

define i1 @"Integer.GreaterEqual%Real%"(ptr %0, double %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %thisReal = sitofp i32 %thisInt to double
  %res = fcmp oge double %thisReal, %1
  ret i1 %res
}

define i1 @"Integer.Equal%Real%"(ptr %0, double %1) {
entry:
  %thisInt = load i32, ptr %0, align 4
  %thisReal = sitofp i32 %thisInt to double
  %res = fcmp oeq double %thisReal, %1
  ret i1 %res
}

define i1 @"Boolean.Or%Boolean%"(ptr %0, i1 %1) {
entry:
  %thisBoolean = load i1, ptr %0, align 1
  %Or = or i1 %thisBoolean, %1
  ret i1 %Or
}

define i1 @"Boolean.And%Boolean%"(ptr %0, i1 %1) {
entry:
  %thisBoolean = load i1, ptr %0, align 1
  %And = and i1 %thisBoolean, %1
  ret i1 %And
}

define i1 @"Boolean.Xor%Boolean%"(ptr %0, i1 %1) {
entry:
  %thisBoolean = load i1, ptr %0, align 1
  %Xor = and i1 %thisBoolean, %1
  ret i1 %Xor
}

define i1 @"Boolean.Not%%"(ptr %0) {
entry:
  %thisBoolean = load i1, ptr %0, align 1
  %1 = xor i1 %thisBoolean, true
  ret i1 %1
}

define i32 @"Boolean.toInteger%%"(ptr %0) {
entry:
  %thisBoolean = load i1, ptr %0, align 1
  %integerResult = select i1 %thisBoolean, i32 1, i32 0
  ret i32 %integerResult
}

define void @"Real%Real%"(ptr %0, double %1) {
entry:
  store double %1, ptr %0, align 8
  ret void
}

define void @"Real%Integer%"(ptr %0, i32 %1) {
entry:
  %res = sitofp i32 %1 to double
  store double %res, ptr %0, align 8
  ret void
}

define i32 @"Real.toInteger%%"(ptr %0) {
entry:
  %thisReal = load double, ptr %0, align 8
  %res = fptosi double %thisReal to i32
  ret i32 %res
}

define double @"Real.UnaryMinus%%"(ptr %0) {
entry:
  %thisReal = load double, ptr %0, align 8
  %res = fneg double %thisReal
  ret double %res
}

define double @"Real.Plus%Real%"(ptr %0, double %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %sum = fadd double %thisReal, %1
  ret double %sum
}

define double @"Real.Plus%Integer%"(ptr %0, i32 %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = sitofp i32 %1 to double
  %sum = fadd double %thisReal, %otherReal
  ret double %sum
}

define double @"Real.Minus%Real%"(ptr %0, double %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %difference = fsub double %thisReal, %1
  ret double %difference
}

define double @"Real.Minus%Integer%"(ptr %0, i32 %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = sitofp i32 %1 to double
  %difference = fsub double %thisReal, %otherReal
  ret double %difference
}

define double @"Real.Mult%Real%"(ptr %0, double %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %multiplication = fmul double %thisReal, %1
  ret double %multiplication
}

define double @"Real.Mult%Integer%"(ptr %0, i32 %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = sitofp i32 %1 to double
  %difference = fmul double %thisReal, %otherReal
  ret double %difference
}

define double @"Real.Div%Real%"(ptr %0, double %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %quotient = fdiv double %thisReal, %1
  ret double %quotient
}

define double @"Real.Div%Integer%"(ptr %0, i32 %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = sitofp i32 %1 to double
  %quotient = fdiv double %thisReal, %otherReal
  ret double %quotient
}

define double @"Real.Rem%Integer%"(ptr %0, i32 %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = sitofp i32 %1 to double
  %reminder = frem double %thisReal, %otherReal
  ret double %reminder
}

define i1 @"Real.Less%Real%"(ptr %0, double %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %res = fcmp ult double %thisReal, %1
  ret i1 %res
}

define i1 @"Real.Less%Integer%"(ptr %0, i32 %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = sitofp i32 %1 to double
  %res = fcmp ult double %thisReal, %otherReal
  ret i1 %res
}

define i1 @"Real.LessEqual%Real%"(ptr %0, ptr %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = load double, ptr %1, align 8
  %res = fcmp ule double %thisReal, %otherReal
  ret i1 %res
}

define i1 @"Real.LessEqual%Integer%"(ptr %0, i32 %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = sitofp i32 %1 to double
  %res = fcmp ule double %thisReal, %otherReal
  ret i1 %res
}

define i1 @"Real.Greater%Real%"(ptr %0, ptr %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = load double, ptr %1, align 8
  %res = fcmp ugt double %thisReal, %otherReal
  ret i1 %res
}

define i1 @"Real.Greater%Integer%"(ptr %0, i32 %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = sitofp i32 %1 to double
  %res = fcmp ugt double %thisReal, %otherReal
  ret i1 %res
}

define i1 @"Real.GreaterEqual%Real%"(ptr %0, ptr %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = load double, ptr %1, align 8
  %res = fcmp uge double %thisReal, %otherReal
  ret i1 %res
}

define i1 @"Real.GreaterEqual%Integer%"(ptr %0, double %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %res = fcmp uge double %thisReal, %1
  ret i1 %res
}

define i1 @"Real.Equal%Real%"(ptr %0, ptr %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = load double, ptr %1, align 8
  %res = fcmp ueq double %thisReal, %otherReal
  ret i1 %res
}

define i1 @"Real.Equal%Integer%"(ptr %0, i32 %1) {
entry:
  %thisReal = load double, ptr %0, align 8
  %otherReal = sitofp i32 %1 to double
  %res = fcmp ueq double %thisReal, %otherReal
  ret i1 %res
}

declare void @printInt(i32)

declare void @printReal(double)

declare void @printBool(i1)

define void @"Console%Integer%"(ptr %0, i32 %1) {
entry:
  call void @printInt(i32 %1)
  ret void
}

define void @"Console%Real%"(ptr %0, double %1) {
entry:
  call void @printReal(double %1)
  ret void
}

define void @"Console%Boolean%"(ptr %0, i1 %1) {
entry:
  call void @printBool(i1 %1)
  ret void
}

define void @"Huy%%"(ptr %0) {
entry:
  %1 = alloca i32, align 4
  call void @"Integer%Integer%"(ptr %1, i32 10)
  %2 = load i32, ptr %1, align 4
  %3 = alloca i32, align 4
  store i32 %2, ptr %3, align 4
  %4 = call i32 @"Integer.Plus%Integer%"(ptr %3, i32 5)
  %5 = alloca i32, align 4
  store i32 %4, ptr %5, align 4
  %6 = call i1 @"Integer.Less%Integer%"(ptr %5, i32 10)
  ret void
}

define void @"Program%Integer,Real,Boolean%"(ptr %0, i32 %1, double %2, i1 %3) {
entry:
  %4 = alloca i32, align 4
  call void @"Console%Integer%"(ptr %4, i32 %1)
  %5 = load i32, ptr %4, align 4
  %6 = alloca i32, align 4
  call void @"Console%Real%"(ptr %6, double %2)
  %7 = load i32, ptr %6, align 4
  %8 = alloca i32, align 4
  call void @"Console%Boolean%"(ptr %8, i1 %3)
  %9 = load i32, ptr %8, align 4
  ret void
}

define void @"Program%Integer,Real,Boolean%.1"(ptr %0, i32 %1, double %2, i1 %3) {
entry:
  ret void
}

define void @"Program.pupa%Integer,Real,Boolean%"(ptr %0, i32 %1, double %2, i1 %3) {
entry:
  ret void
}
