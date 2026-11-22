export interface Address {
  id: number;
  governorate: string;
  city: string;
  street: string;
}

export interface Category {
  id: number;
  name: string;
  description?: string;
}

export interface Governorate {
  id: number;
  arabicName: string;
  englishName?: string;
  provinceId?: number;
}

export interface City {
  id: number;
  arabicName: string;
  englishName?: string;
  governorateId: number;
}

export interface AuthUser {
  id: number;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  nationalId: number;
  gender: string;
  birthDate: string;
  createdAt?: string;
  bio?: string;
  isOnline: boolean;
  isShop: boolean;
  shopName?: string;
  categoryId?: number;
  categoryName?: string;
  experienceYears?: number;
  price?: number;
  rating?: number;
  roles: string[];
  role: string;
  profilePictureUrl?: string;
  address?: Address | null;
}

export interface TechnicianCardItem {
  id: number;
  userName: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  bio?: string;
  isOnline: boolean;
  isShop: boolean;
  shopName?: string;
  categoryId?: number;
  categoryName: string;
  experienceYears: number;
  price: number;
  rating: number;
  reviewCount: number;
  profilePictureUrl?: string;
  governorate: string;
  city: string;
  street: string;
  address?: Address | null;
}

export interface ReviewItem {
  id: number;
  score: number;
  comment: string;
  createdAt: string;
  clientId: number;
  clientName: string;
}

export interface TechnicianDetails extends TechnicianCardItem {
  createdAt: string;
  averageRating: number;
  ratings: ReviewItem[];
}

export interface TechnicianScheduleSlot {
  id: number;
  technicianId: number;
  date: string;
  startTime: string;
  endTime: string;
  isBooked: boolean;
}

export interface TaskDto {
  id: number;
  title: string;
  description: string;
  status: string;
  date?: string;
  otherPartyName: string;
  otherPartyId?: number;
  otherPartyUserName?: string;
  price: number;
  serviceName: string;
  paymentMethodId?: number;
}

export interface DashboardUserProfile {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  bio?: string;
  location: string;
  profilePictureUrl?: string;
  serviceDetails?: string;
  shopName?: string;
  isShop?: boolean;
}

export interface CustomerDashboardDto {
  profile: DashboardUserProfile;
  pastBookings: TaskDto[];
  upcomingAppointments: TaskDto[];
}

export interface WorkerDashboardDto {
  profile: DashboardUserProfile;
  receivedRequests: TaskDto[];
  acceptedJobs: TaskDto[];
  totalEarnings: number;
}

export interface ChatMessage {
  id?: number;
  senderId: number;
  receiverId: number;
  content: string;
  sentAt: string;
}

export interface ChatPartner {
  id: number;
  userName: string;
  firstName: string;
  lastName: string;
  isOnline: boolean;
  shopName?: string;
  isShop?: boolean;
  profilePicture?: {
    id: number;
    path: string;
  } | null;
}

export interface AdminStats {
  totalUsers: number;
  totalTaskers: number;
  totalStores: number;
  totalCategories: number;
  topCategories: { id: number; name: string; count: number }[];
}
