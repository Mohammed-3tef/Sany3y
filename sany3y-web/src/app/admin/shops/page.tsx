import React from "react";
import { AdminUsersTable } from "@/components/AdminUsersTable";

export default function AdminShopsPage() {
  return <AdminUsersTable onlyShops={true} />;
}
